using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using System.Drawing;

namespace WebAPIDemo.Controllers
{
    public class FileUploadController : ApiController
    {


        CustomerEntities db = new CustomerEntities();

        [HttpPost]
        public async Task<HttpResponseMessage> Upload(int projectId, int sectionId)
        {
            var status = new MyReponse();
            try
            {
                var context = HttpContext.Current.Request;
                if (context.Files.Count > 0)
                {
                    var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();
                    var index = 0;
                    foreach (var streamContent in filesReadToProvider.Contents)
                    {
                        var fileBytes = await streamContent.ReadAsByteArrayAsync();
                        var file = new File();
                        file.ProjectId = projectId;
                        file.SectionId = sectionId;
                        file.FileName = context.Files[index].FileName;
                        file.FileSize = fileBytes.Length;
                        file.ImagePath = String.Format("/UploadedFiles/{0}_{1}_{2}", projectId, sectionId, file.FileName);
                        file.ThumbPath = String.Format("/UploadedFiles/{0}_{1}_th_{2}", projectId, sectionId, file.FileName);
                        var img = Image.FromStream(new System.IO.MemoryStream(fileBytes));
                        await SaveFiles(file, img);
                        index++;
                    }
                    status.Status = true;
                    status.Message = "File uploaded successfully";
                    return Request.CreateResponse(HttpStatusCode.OK, status);
                }
            }
            catch (Exception ex)
            {
                status.Message = ex.Message;
            }
            return Request.CreateResponse(HttpStatusCode.OK, status);
        }

        private async Task SaveFiles(File file, Image img)
        {
            // save thumb
            SaveToFolder(img, new Size(160, 160), file.ThumbPath);
            // save image of size max 600 x 600
            SaveToFolder(img, new Size(600, 600), file.ImagePath);
            // Save  to database
            await Save(file);
        }
        public async Task<HttpResponseMessage> GetImages()
        {
            var response = new MyReponse();
            var files = await db.Files.ToListAsync();          
            response.Result = files;
            response.Status = true;
            response.Message = "Success";
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        private async Task<bool> Save(File file)
        {
            db.Files.Add(file);
            await db.SaveChangesAsync();
            return true;
        }

        private Size NewImageSize(Size imageSize, Size newSize)
        {
            Size finalSize;
            double tempval;
            if (imageSize.Height > newSize.Height || imageSize.Width > newSize.Width)
            {
                if (imageSize.Height > imageSize.Width)
                    tempval = newSize.Height / (imageSize.Height * 1.0);
                else
                    tempval = newSize.Width / (imageSize.Width * 1.0);

                finalSize = new Size((int)(tempval * imageSize.Width), (int)(tempval * imageSize.Height));
            }
            else
                finalSize = imageSize; // image is already small size

            return finalSize;
        }

        private void SaveToFolder(Image img, Size newSize, string pathToSave)
        {
            // Get new resolution
            Size imgSize = NewImageSize(img.Size, newSize);
            using (System.Drawing.Image newImg = new Bitmap(img, imgSize.Width, imgSize.Height))
            {
                // Remove image if already exist and save again
                if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(pathToSave)))
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath(pathToSave));

                newImg.Save(HttpContext.Current.Server.MapPath(pathToSave), img.RawFormat);                
            }
        }

    }

    public class MyReponse
    {
        public Boolean Status { get; set; }
        public String Message { get; set; }
        public Object Result { get; set; }

        public MyReponse()
        {
            this.Status = false;
            this.Message = "Some internal error";
        }
    }
}
