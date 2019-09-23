-----------------------
-- Structure tables. --
-----------------------

-- A fileset is a top-level grouping of files, similar to a normal namespace in Mediawiki.
create table filesets(
    id               INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name             VARCHAR(63) NOT NULL,
    root_inode       INTEGER     NOT NULL REFERENCES inodes,
    default_security INTEGER     NOT NULL REFERENCES descriptors,

    UNIQUE(name)
);

-- Indexes manifest as pairs of INDEX/NAMES attributes. Which ones you can have is specified at the
-- fileset level. They provide subpage and category functions. Depending on the flavour, they may
-- logically exist on the container or the contents; filesystem-like behaviour is the former, wiki-
-- category-like behaviour is the latter (this mainly influences security processing).
create table indexes(
    id               INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    fileset          INTEGER     NOT NULL REFERENCES filesets,
    container_name   VARCHAR(63) NOT NULL,
    namelist_name    VARCHAR(63) NOT NULL,
    logically_on     ENUM('container', 'member') NOT NULL,
    important_names  BOOLEAN     NOT NULL,

    CHECK (container_name != namelist_name),
    UNIQUE(container_name),
    UNIQUE(namelist_name),
);

-- An inode is a file. Not its names, just the file itself.
create table inodes(
    id                 INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    fileset            INTEGER     NOT NULL REFERENCES filesets,
    created_by         INTEGER     NOT NULL REFERENCES principals,
    created_at         TIMESTAMP   NOT NULL,
    last_modified_by   INTEGER     NOT NULL REFERENCES principals,
    last_modified_at   TIMESTAMP   NOT NULL,
    change_description TEXT        NOT NULL,
    display_name       TEXT        NOT NULL
);

-- Lists which dirent to use to create a canonical URL or breadcrumbs or whatever.
create table canonical_dirents (
    index  INTEGER NOT NULL REFERENCES indexes,
    inode  INTEGER NOT NULL REFERENCES inodes,
    dirent INTEGER NOT NULL REFERENCES dirents,
    UNIQUE(index, inode)
);

-- A dirent is the name of a file, or membership in a category, or the like. Hard dirents are just
-- regular hardlinks. Soft and URL ones generate a 3xx when viewed through HTTP, differing on their
-- means for determining the Location.
create table dirents(
    id          INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    directory   INTEGER     NOT NULL REFERENCES inode,
    index       INTEGER     NOT NULL REFERENCES indexes,
    index_attr  INTEGER     NOT NULL REFERENCES index_attributes, -- these two don't need to exist. they're just for constraints.dsdsdsd
    name_attr   INTEGER     NOT NULL REFERENCES name_attributes,
    slug        VARCHAR     NOT NULL,
    sort_key    VARCHAR     NOT NULL, 
    target_node INTEGER              REFERENCES inode,
    target_url  TEXT,
    kind ENUM('hard', 'soft', 'url') NOT NULL,

    CHECK((kind = 'url') != (target_url IS NULL)),
    UNIQUE(index, directory, slug)
);

--------------------
-- Content tables --
--------------------

-- Base table for attributes. 
create table attributes(
    id          INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    inode       INTEGER NOT NULL REFERENCES inodes,
    name        VARCHAR NOT NULL,
    type        ENUM('data', 'keyvalues', 'descriptor', 'index', 'names'),

    UNIQUE(inode, name)
);

-- A big blob of data. Stored out of line just in case we decide that a gargantuan size limit is
-- fine and someone uses a Range header or TUS.
create table data_attributes(
    id INTEGER NOT NULL REFERENCES attributes,
    type INTEGER NOT NULL REFERENCES attribute_types DEFAULT 'data'
      
);

create table keyvalues_attributes();
create table keyvalues_records();
create table descriptors();