import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import {Observable} from 'rxjs/Rx';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

@Injectable()
export class FileService {
    _baseURL: string = 'http://localhost:33699/api/fileupload/'
    constructor(private http: Http) { }

    upload(files, parameters){      

        let headers = new Headers();
        let options = new RequestOptions({ headers: headers });
        options.params = parameters;
        return  this.http.post(this._baseURL + 'upload', files, options)
                 .map(response => response.json())
                 .catch(error => Observable.throw(error));
                  
    }


    getImages(){
        return this.http.get(this._baseURL + "getimages")
                   .map(response => response.json())
                   .catch(error => Observable.throw(error));
    }
}
