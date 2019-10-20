import * as stream from 'stream';

export type AsyncResult<T, E extends Error = Error> = Promise<T|E>;
export type Principal = string;

export interface Volume {
    label : string;
    type  : "wiki";
    rootInode : Promise<Inode>;
    resolvePath(path: string) : Promise<Inode>;
    resolveInode(inodeId: string) : Promise<Inode>;
};

export interface UnlockedVolume extends Volume {
    beginNewVersion(principal: Principal) : Promise<WriteableVolume>;
}

export interface WriteableVolume extends Volume {
    commit() : Promise<void>;
    abandon() : Promise<void>;

    newInode() : Promise<WriteableInode>;

    rootInode : Promise<WriteableInode>;
    resolvePath(path: string) : Promise<WriteableInode>;
    resolveInode(inodeId: string) : Promise<WriteableInode>;
}

export interface Inode {
    id: string;
    getAttribute(name: string) : Promise<Attribute>;
    traverse(index: string, slug: string) : Promise<Inode>;
};

export interface WriteableInode extends Inode {
    putAttribute(name: string, newMime: string, newData : any) : Promise<void>;
    traverse(index: string, slug: string) : Promise<WriteableInode>;
    unlinkChild(index: string, slug: string) : Promise<void>;
    link(index: string, newLink: LinkInfo) : Promise<void>;
}

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
    
    get(key: string) : Promise<any>;
}