-- This needs PG 12 for the GENERATED columns.

CREATE SCHEMA wiki;

CREATE TYPE wiki.dirent_type    ENUM('hard', 'soft', 'url');
CREATE TYPE wiki.index_type     ENUM('category', 'directory');
CREATE TYPE wiki.attribute_type ENUM('data', 'atom', 'keyvalues');
CREATE TYPE wiki.search_data    TEXT

CREATE TABLE wiki.volumes (
    id           INTEGER     PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name         VARCHAR(63) NOT NULL,
    root_inode   INTEGER     NOT NULL    REFERENCES wiki.inodes
);

CREATE TABLE wiki.inodes (
    id           INTEGER      PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    volume       INTEGER      NOT NULL    REFERENCES wiki.volumes,
    title        VARCHAR(127) NOT NULL, -- It's good enough for Darwin, it's good enough for you.
    canonical    INTEGER      NOT NULL    REFERENCES wiki.dirents
);

CREATE TABLE wiki.dirents(
    id           INTEGER          PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    container    INTEGER          NOT NULL    REFERENCES wiki.inodes,
    entry_type   wiki.dirent_type NOT NULL,
    index_type   wiki.index_type  NOT NULL,
    target_node  INTEGER                      REFERENCES wiki.inodes,
    target_url   VARCHAR,
    slug         VARCHAR(63)      NOT NULL,
    sort_key     VARCHAR          NOT NULL,

    UNIQUE(container,slug),
    CHECK((target_url IS NOT NULL) = (type = 'url')),
    CHECK((target_node IS NOT NULL) = (type != 'url'))
);

-- These three tables implement a table inheritance scheme such that attribute names are unique per inode,
-- and each attribute is of one type.

CREATE TABLE wiki.attributes(
    id           INTEGER             PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    inode        INTEGER             NOT NULL    REFERENCES wiki.inodes,
    name         VARCHAR(63)         NOT NULL,
    type         wiki.attribute_type NOT NULL,
    search_data  wiki.search_data    NOT NULL,

    UNIQUE(inode, name), -- Names are unique per inode
    UNIQUE(id, type),    -- pgsql 12 docs § 5.4.5 "A foreign key must reference columns that either are a primary key or form a unique constraint."
    CHECK( name NOT IN ('children', 'category-members','parents', 'categories') ) -- These are virtual in the application.
);

CREATE TABLE wiki.data_attributes(
    id           INTEGER             PRIMARY KEY REFERENCES wiki.attributes (id),
    type         wiki.attribute_type NOT NULL    REFERENCES wiki.attributes (type) GENERATED ALWAYS AS ('data') STORED,
    path         TEXT                NOT NULL,
    content_type TEXT                NOT NULL,

    FOREIGN KEY (id, type) REFERENCES wiki.attributes (id, type),
    CHECK (type = 'data')
);

CREATE TABLE wiki.atom_attributes(
    id           INTEGER             PRIMARY KEY REFERENCES wiki.attributes (id),
    type         wiki.attribute_type NOT NULL    REFERENCES wiki.attributes (type) GENERATED ALWAYS AS ('atom') STORED,
    content      TEXT                NOT NULL, -- should this be always json?
    content_type TEXT                NOT NULL,

    FOREIGN KEY (id, type) REFERENCES wiki.attributes (id, type),
    CHECK (type = 'atom')
);


CREATE TABLE wiki.keyvalue_attributes(
    id           INTEGER             PRIMARY KEY REFERENCES wiki.attributes (id) ,
    type         wiki.attribute_type NOT NULL    REFERENCES wiki.attributes (type) GENERATED ALWAYS AS ('keyvalues') STORED,
);

CREATE TABLE wiki.keyvalue_entries(
    attribute    INTEGER NOT NULL REFERENCES wiki.keyvalue_attributes(id),
    key          TEXT    NOT NULL,
    value        JSONB   NOT NULL, -- TODO: Should this be text?
);
