import * as Request from './request';
import * as Response from './response';
import { Stream } from 'stream';

type RevisionType = "manual" | "bot" | "workspace_autopublish";
type WriteTransaction = Writable & NewRevision;

export interface VolumeInfo {
    id: number;
    name: string;
}

export interface Readable {
    handleRequest(req: Request.Get) : Promise<Response.Basic>;
}

export interface Writable extends Readable {
    handleRequest(req: Request.Basic, body?: Stream) : Promise<Response.Basic>;
}

export interface Versioned {
    currentVersionId() : string;
    atVersion(treeish: string) : Readable;
    beginRevision(treeish: string) : WriteTransaction;
    beginCherryPickMerge(source: string, dest: string) : CherryPicker;
    getBranches() : string[];
}

export interface NewRevision {
    commit(message: string, type: RevisionType) : any;
}

export interface CherryPicker extends NewRevision {
    pick(path: string, srcrev?: string) : void;
}

export interface VolumeProvider {
    getVolumeNames() : Promise<string[]>;
    getVolume(volume: string) : Promise<(VolumeInfo & Readable) | null>;
}