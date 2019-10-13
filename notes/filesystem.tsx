type AsyncResult<T, E extends Error = Error> = Promise<T|E>;

interface Volume {
    name: string;
    type: string;
    getInode(what: string | number) : AsyncResult<Inode>;
    getPath(what: string | string[]) : AsyncResult<Inode>;
};

type attributeType = "data" | "keyvalues" | "index" | "entries" | "atom";

interface Inode {
    id: number;
    attribute(name: string, expectedType: attributeType) : Attribute,
    renameAttribute(oldName: string, newName: string) : Promise<void>,
    removeAttribute(attr: string) : Promise<void>,
}

interface Attribute {
    type: attributeType;
}

interface AttributeCoreStats {
    name: string;
    type: attributeType;
    size: number;
    contentType: string;
    modified: Date;
    etag?: string;
}

type AttributeResponse<T> = AttributeCoreStats & {stream: T};

interface DataAttribute extends Attribute {
    type: "data",
    read(start: number, length?: number) : AsyncResult<AttributeResponse<ReadableStream>>;
    replace(data: Buffer|string|ReadableStream, newType?: string) : AsyncResult<void>;
    patch(data: Buffer|string|ReadableStream, newType?: string) : AsyncResult<void>;
    stat() : AsyncResult<AttributeCoreStats>;
}

interface AtomAttribute extends Attribute {
    type: "atom",
    read(start: number, length?: number) : AsyncResult<AttributeResponse<ReadableStream>>;
    replace(data: Buffer|string|ReadableStream, newType?: string) : AsyncResult<void>;
    stat() : AsyncResult<AttributeCoreStats>;
}

interface KeyvaluesAttribute extends Attribute {
    type: "keyvalues",
    read(items?: string[]) : AsyncResult<AttributeResponse<KeyvaluesStream>>;
    replace(newitems: KeyvaluesStream) : AsyncResult<void>;
    patch(newitems: KeyvaluesStream) : AsyncResult<void>;
}

type KeyvaluesStream = AsyncIterableIterator<[string, any][]>;

interface IndexAttribute extends Attribute {
    type: "index" | "entries",
    read(start: number, length?: number) : AsyncResult<AttributeResponse<AsyncIterableIterator<IndexItem>>>;
    patch(newitems: AsyncIterableIterator<IndexOp>) : AsyncResult<void>;
}

type IndexOp = {op: "link", item: Omit<IndexItem, "id">} | {op: "overwrite", item: Partial<IndexItem>} | {op: "unlink", id: number};

type IndexItem = InodeIndexItem | UrlIndexItem;

interface BaseIndexItem {
    type: "hard" | "soft" | "url";
    id: number;
    slug: string;
    container: { volume: string, inode: number };
    sortKey: string;
    canonical: boolean;
}

interface InodeIndexItem extends BaseIndexItem {
    type: "hard"|"soft",
    target: { volume: string, inode: number };
}

interface UrlIndexItem extends BaseIndexItem {
    type: "url",
    target: string
}