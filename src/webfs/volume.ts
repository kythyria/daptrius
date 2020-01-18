import * as Request from './request';
import { Stream } from 'stream';

type Response = any

export interface Readable {
    handleRequest(req: Request.Get) : Response;
}

export interface Writable extends Readable {
    handleRequest(req: Request.Basic, body?: Stream) : Response;
}

export interface Versioned {
    currentVersionId() : string;
    atVersion(branch: string, verid : string) : Readable;
    beginRevision(branch: string, verid: string) : Writable & NewRevision;
    beginCherryPickMerge(source: string, dest: string) : CherryPicker;
    getHistory(path: string) : Response;
}

export interface NewRevision {
    commit(message: string) : any;
}

export interface CherryPicker {
    commit(message: string) : any;
    pick(path: string, srcrev?: string) : void;
}