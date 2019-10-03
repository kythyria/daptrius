-----------------------
-- Structure tables. --
-----------------------

-- A fileset is a top-level grouping of files, similar to a normal namespace in Mediawiki.
create table filesets(
    id               INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name             VARCHAR(63) NOT NULL,
    root_inode       INTEGER     NOT NULL REFERENCES inodes

    UNIQUE(name)
);

-- An inode is a file. Not its names, just the file itself.
create table inodes(
    id                 INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    fileset            INTEGER     NOT NULL REFERENCES filesets,
    revision           INTEGER     NOT NULL REFERENCES inode_revisions
    change_description TEXT        NOT NULL,
    display_name       TEXT        NOT NULL,
    protection         ENUM('unprotected', 'protected', 'immutable') NOT NULL
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
    slug        VARCHAR     NOT NULL,
    sort_key    VARCHAR     NOT NULL, 
    target_node INTEGER              REFERENCES inode,
    target_url  TEXT,
    index_type  ENUM('category', 'directory') NOT NULL,
    kind        ENUM('hard', 'soft', 'url')   NOT NULL,

    CHECK((kind = 'url') != (target_url IS NULL)),
    UNIQUE(index_type, directory, slug)
);

--------------------
-- Content tables --
--------------------

-- Types of attribute.
create type attribute_type ENUM(
    'binary',     -- binary data
    'text',       -- indexed plain text or markup
    'keyvalues',  -- key-value pairs, key is string, value is json
    'descriptor', -- TODO: What is this? JSONB?
    'index',      -- Unused (all such are virtual, provided by the app)
    'names'       -- Unused (ditto)
);

-- Base table for attributes. 
create table attributes(
    id          INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    inode       INTEGER        NOT NULL REFERENCES inodes,
    name        VARCHAR        NOT NULL,
    type        attribute_type NOT NULL,

    UNIQUE(inode, name),
    UNIQUE(id, type)
    CHECK( name NOT IN ('children', 'category-members','parents', 'categories') ),
    CHECK( type NOT IN ('index', 'names') )
);

-- A blob of data. The actual data should really be outside the filesystem, but postgres docs say
-- not to worry below a few megabytes. Also TODO: full-text search.
create table binary_attributes(
    id       INTEGER        NOT NULL REFERENCES attributes,
    type     attribute_type NOT NULL DEFAULT 'binary',
    mime     TEXT           NOT NULL,
    data     BYTEA,
      
    FOREIGN KEY (id, type) references attributes (id, type),
    CHECK (attribute_type = 'binary')
);

create table text_attributes(
    id          INTEGER        NOT NULL REFERENCES attributes,
    type        attribute_type NOT NULL DEFAULT 'text',
    mime        TEXT           NOT NULL,
    data        TEXT           NOT NULL,
    search_data TEXT           NOT NULL,
    
    FOREIGN KEY (id, type) references attributes (id, type),
    CHECK (attribute_type = 'text')
)

-- Key-value pairs. We don't use HSTORE here because we want to paginate, and it'll be expensive
-- to do revision control underneath.
create table keyvalue_attributes(
    id   INTEGER        NOT NULL REFERENCES attributes PRIMARY KEY,
    type attribute_type NOT NULL DEFAULT 'keyvalues',

    FOREIGN KEY (id, type) references attributes (id, type),
    CHECK (attribute_type = 'keyvalues'),
);

create table keyvalue_entries(
    attribute INTEGER NOT NULL REFERENCES keyvalue_attributes,
    key       TEXT    NOT NULL,
    value     JSONB   NOT NULL
);

--------------
-- Security --
--------------

create table principals (
    id          INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    handle      TEXT     
);