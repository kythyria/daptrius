import * as vol from '../webfs/volume';
import * as Request from '../webfs/request';
import * as Response from '../webfs/response';
import * as pg from 'pg';


class Provider implements vol.VolumeProvider {
    db : pg.ClientBase;

    constructor(connection : pg.ClientBase) {
        this.db = connection;
    }

    async getVolumeNames() : Promise<string[]> {
        let res = await this.db.query("SELECT name FROM volumes;");
        return res.rows.map(i => i.name);
    }

    async getVolume(name: string) : Promise<(vol.VolumeInfo & vol.Readable) | null> {
        let res = await this.db.query("SELECT id, name FROM volumes WHERE name = $1;", [name]);
        if(res.rows.length > 1) {
            throw new Error("Database is corrupt (multiple volumes with the same name)!");
        }
        else if(res.rows.length == 0) {
            return null;
        }

        return new Volume(this.db, res.rows[0]);
    }
}

class Volume implements vol.VolumeInfo, vol.Readable {
    db : pg.ClientBase;
    id: number;
    name: string;
    revision: number;

    constructor(db: pg.ClientBase, vi: vol.VolumeInfo) {
        this.db = db;
        this.id = vi.id;
        this.name = vi.name;
        this.revision = Number.POSITIVE_INFINITY;
    }

    async handleRequest(req: Request.Get) : Promise<Response.Basic> {
        let inodeId : number;
        if(typeof req.path == "number") {
            inodeId = req.path;      
        }
        else {
            let res = await this.db.query(`
                WITH RECURSIVE cte AS (
                    SELECT id, parent, $1::text[] AS path, 2 AS level FROM inodes
                    WHERE parent IS NULL AND first_rev <= $2 AND last_rev >= $2 AND slug = $1[0] AND volume = $3
                    UNION ALL
                    SELECT i.id, i.parent, cte.path, cte.level + 1
                    FROM cte 
                    JOIN inodes i ON i.parent = cte.id AND i.slug = cte.path[level]
                    WHERE i.first_rev <= $2 AND i.last_rev >= $2
                )
                SELECT id FROM cte ORDER BY cte.level DESC;`,
                [
                    req.path,
                    this.revision,
                    this.id
                ]
            );
            if(res.rows.length != req.path.length) {
                return {
                    status: "notFound",
                    path: req.path
                }
            }
        }
    }
}