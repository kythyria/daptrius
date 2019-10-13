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

CREATE TABLE wiki.revisions (
    id             INTEGER        PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    revisor        wiki.principal NOT NULL,
    created        TIMESTAMP      NOT NULL,
    message        TEXT,
    change_summary TEXT
);

CREATE TABLE wiki.inodes (
    id           INTEGER      NOT NULL GENERATED ALWAYS AS IDENTITY,
    volume       INTEGER      NOT NULL REFERENCES wiki.volumes,
    revision     INTEGER      NOT NULL REFERENCES wiki.revisions,
    title        VARCHAR(127) NOT NULL, -- It's good enough for Darwin, it's good enough for you.
    canonical    INTEGER      NOT NULL REFERENCES wiki.dirents,

    PRIMARY KEY (id, revision)
);

CREATE TABLE wiki.dirents (
    id           INTEGER          NOT NULL GENERATED ALWAYS AS IDENTITY,
    revisions    INT4RANGE        NOT NULL,
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
)