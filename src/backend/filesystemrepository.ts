import * as WikiFS from './wikifs';
import { promises as fs } from 'fs';
import * as path from 'path';

export class FilesystemRepository implements WikiFS.Volume {
    type : "wiki" = "wiki";
    rootInodeId : string;
    inodeOwnsDirentsIn : string[];

    storagePath : string;
    
    /* throws if it can't read or decode volumeinfo */
    static async open(storagePath: string) : Promise<FilesystemRepository> {
        let vipath = path.join(storagePath, "volumeinfo");
        let chars = <string>(await fs.readFile(vipath, { encoding: "UTF-8" }));
        let vi = JSON.parse(chars);

        if(typeof vi.root != "string") {
            throw new Error(`Can't decode \"${vipath}\": root inode ID not a string`);
        }

        let dbtif = vi.direntsBelongToInodeFor;

        if(!Array.isArray(dbtif)) {
            throw new Error(`Can't decode \"${vipath}\": index direction list not an array`);
        }

        if(!dbtif.every(i => typeof i == "string")) {
            throw new Error (`Can't decode \"${vipath}\": index direction list includes nonstring`);
        }

        return new FilesystemRepository(storagePath, vi.root, dbtif)
    }

    constructor(storagePath: string, rootInodeId : string, inodeOwnsDirentsIn : string[]) {
        this.storagePath = storagePath;
        this.rootInodeId = rootInodeId;
        this.inodeOwnsDirentsIn = inodeOwnsDirentsIn;
    }

    rootInode() : WikiFS.AsyncResult<WikiFS.Inode> {
        return this.resolveInode(this.rootInodeId);
    }

    async resolvePath(path: string[] | string) : WikiFS.AsyncResult<WikiFS.Inode> {
        let segments : string[];
        if(typeof path == "string") {
            segments = path.split("/");
        }
        else {
            segments = path;
        }

        segments = segments.reduce<string[]>( (m,v) => {
            switch(v) {
                case ".":
                case "":
                    break;
                case "..":
                    m.pop();
                    break;
                default:
                    m.push(v);
                    break;
            }
            return m;
        }, []);

        let curr = await this.rootInode();
        if(curr instanceof Error) {
            return curr;
        }
        for(let i of segments) {
            let next = await curr.traverse("children", i);
            if(curr instanceof Error) {
                return curr;
            }
        }
        return curr;
    }

    resolveInode(inodeId: string) : WikiFS.AsyncResult<WikiFS.Inode> {
        return FilesystemInode.open(this, inodeId);
    }
}

export class FilesystemInode {
    static async open(fsr: FilesystemRepository, id: string) : WikiFS.AsyncResult<WikiFS.Inode> {
        let infopath = path.join(fsr.storagePath, "inodes", id, "basicinfo-inode");
        let infobytes: string;
        let info : any;
        try {
            infobytes = <string>(await fs.readFile(infopath, {encoding: "UTF-8"}));
            info = JSON.parse(infobytes);
        }
        catch(e) {
            return new FsError.InodeNotFound(id, e);
        }

        if(isValidBasicInfo(info)) {
            return new FilesystemInode(fsr, id, info);
        }
        else {
            return new FsError.BasicInfoInvalid(id);
        }
    }

    
}