import { Volume, Principal, Inode, LinkInfo, AsyncResult } from "./wikifs";

export interface UnlockedVolume extends Volume {
    beginNewVersion(principal: Principal) : AsyncResult<WriteableVolume>;
}

export interface WriteableVolume extends Volume {
    commit() : AsyncResult<void>;
    abandon() : AsyncResult<void>;

    newInode() : AsyncResult<WriteableInode>;

    rootInode() : AsyncResult<WriteableInode>;
    resolvePath(path: string) : AsyncResult<WriteableInode>;
    resolvePath(path: string[]) : AsyncResult<WriteableInode>;
    resolveInode(inodeId: string) : AsyncResult<WriteableInode>;
}

export interface WriteableInode extends Inode {
    putAttribute(name: string, newMime: string, newData : any) : AsyncResult<void>;
    traverse(index: string, slug: string) : AsyncResult<WriteableInode>;
    unlinkChild(index: string, slug: string) : AsyncResult<void>;
    link(index: string, newLink: LinkInfo) : AsyncResult<void>;
}