import { Volume, Principal, Inode, LinkInfo } from "./wikifs";

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

export interface WriteableInode extends Inode {
    putAttribute(name: string, newMime: string, newData : any) : Promise<void>;
    traverse(index: string, slug: string) : Promise<WriteableInode>;
    unlinkChild(index: string, slug: string) : Promise<void>;
    link(index: string, newLink: LinkInfo) : Promise<void>;
}