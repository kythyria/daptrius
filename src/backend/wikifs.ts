import * as stream from 'stream';

export type AsyncResult<T, E extends Error = Error> = Promise<T|E>;
export const AsyncResult = Promise // Fixes ts(1055) errors.
export type Principal = string;

export interface Volume {
    type  : "wiki";
    rootInode() : AsyncResult<Inode>;
    resolvePath(path: string) : AsyncResult<Inode>;
    resolvePath(path: string[]) : AsyncResult<Inode>;
    resolveInode(inodeId: string) : AsyncResult<Inode>;
};

export interface Inode {
    id: string;
    getAttribute(name: string) : AsyncResult<Attribute>;
    traverse(index: string, slug: string) : AsyncResult<Inode>;
};

export interface LinkInfo {
    target: {volume?: string, inode: string} | {url: string},
    sortKey: string,
    slug: string,
    type: "redirect" | "hardlink"
}

export interface RedirectLinkInfo extends LinkInfo {
    type: "redirect",
    target: {volume?: string, inode: string} | {url: string},
}

export interface HardLinkInfo extends LinkInfo {
    type: "hardlink",
    target: {volume?: string, inode: string}
}

export interface Attribute {
    type: string;
    name: string;
}

export interface DataAttribute extends Attribute {
    type: "data";
    mime: string;
    contents: stream.Readable;
}

export interface BasicInfoAttribute extends Attribute {
    type: "basicinfo";
    title: string;
    canonicalPath: string;
}

export interface KeyValuesAttribute extends Attribute, AsyncIterable<any> {
    type: "keyvalues";
    
    get(key: string) : AsyncResult<any>;
}