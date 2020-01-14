import * as Stream from 'stream';

interface Volume {
    current(): ReadableVolume;
    atRevision(rev: string): ReadableVolume;
    beginWriteTransaction(rev: string) : WritableVolume;
}

interface ReadableVolume {
    getStream(path: string, attribute: string, seek: number) : StreamResult;
    getObjectList(path: string, attribute: string, count: number, resumeToken?: string) : ItemResult<Dirent>;
    getProperties(path: string, attribute: string, propertyNames: string[]) : ItemResult<Property>;
    getAllProperties(path: string, attribute: string, count: number, resumeToken?: string) : ItemResult<Property>;
}

/*
vol.current().path("/foo/bar").getStream("data");
vol.current().path("/foo/bar").getProperties(["fs.owner"]);
*/