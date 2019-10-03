type AsyncResult<T, E extends Error = Error> = Promise<T|E>;

interface Volume {
    
};

type attributeType = "bytes" | "keyvalues" | "index";

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

interface BytesAttribute extends Attribute {
    type: "bytes",
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
    type: "index",
    read(start: number, length?: number) : AsyncResult<AttributeResponse<AsyncIterableIterator<IndexItem>>>;
}

interface IndexItem {
    type: "hard" | "soft" | "url"
    slug: string;
    target: number|string;
    container: number;
}