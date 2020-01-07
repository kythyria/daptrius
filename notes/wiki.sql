-----------
-- Users --
-----------
-- Notice there's no actual way to log in here. That is handled by another module the job
-- of which is to actually do the login and spawn a token containing authorisation to act
-- as the relevant principal. That module even looks after cookies etc.

-- A principal is a user or group. The moniker and avatar are just for user convenience.
-- In most circumstances they would be automatically populated from an external source,
-- this is just a cache, since the external source would be unhappy if we hit it too often.
CREATE TABLE principals (
    id            INTEGER     PRIMARY KEY,
    local_moniker VARCHAR(64) NOT NULL,
    local_avatar  BYTEA,
    enabled       BOOLEAN     NOT NULL, -- Disabled principals don't count. Can't log in, can't be used as grounds for authz.
    is_identity   BOOLEAN     NOT NULL  -- Can you login/sudo as this principal?
);

-- Group and sudoers memberships.
CREATE TABLE principal_authorisations (
    principal_id       INTEGER NOT NULL REFERENCES(principals.id),
    actor_principal_id INTEGER NOT NULL REFERENCES(principals.id)
);

------------------
-- Core content --
------------------
-- Since every page has one canonical path, dirents can be inodes. Redirects then have to
-- be a special type of stream, but that's no worse than existing systems, eg Windows has
-- shell links be a file format/extension that's special cased by the shell. Probably not
-- useful that redirects thus have their own read flags, but we can live with that, maybe
-- put something in the UI to object to the sillier things. This does allow pages to have
-- descendants but no content, but that's fine too. Filesystems do that, after all.

-- One row for each distinct "commit". The approver is for review workflows where another
-- user must approve a revision before it becomes visible to everyone.
CREATE TABLE revisions (
    id             INTEGER   PRIMARY KEY,
    created_at     TIMESTAMP NOT NULL,
    creator_ident  INTEGER   NOT NULL REFERENCES(principals.id),
    approved_at    TIMESTAMP NOT NULL,
    approver_ident INTEGER   NOT NULL REFERENCES(principals.id),
    change_message TEXT      NOT NULL,
    source         ENUM('manual', 'bot', 'workspace_autopublish') NOT NULL
);

CREATE TABLE inodes (
    id         INTEGER PRIMARY KEY,
    first_rev  INTEGER NOT NULL,
    last_rev   INTEGER NOT NULL,
    slug       VARCHAR NOT NULL, -- Path component. Ones for a page are often called slugs in web-land.
    sortkey    VARCHAR,          -- If set, used instead of slug when sorting by slug.
    title      VARCHAR NOT NULL, -- For use in <title> elements etc.
    parent     INTEGER          REFERENCES(inodes.id),
);

CREATE TABLE streams (
    first_rev       INTEGER  NOT NULL,
    last_rev        INTEGER  NOT NULL,
    inode_id        INTEGER  NOT NULL REFERENCES(inodes.id),
    stream_name     VARCHAR  NOT NULL,
    mime            VARCHAR  NOT NULL,
    content         BYTEA    NOT NULL  -- todo: Better type. Or multiple tables, or just a bunch of different nullable fields?
);

CREATE TABLE properties (
    first_rev       INTEGER  NOT NULL,
    last_rev        INTEGER  NOT NULL,
    inode_id        INTEGER  NOT NULL REFERENCES(inodes.id),
    prop_name       VARCHAR  NOT NULL,
    content         JSONB    NOT NULL,
);

--------------------
-- Content caches --
--------------------
-- Both of these can be regeneraed from the other tables, but doing so is expensive, it'd require rendering.

-- What Links Here and categories.
CREATE TABLE crossreferences (
    first_rev   INTEGER NOT NULL,
    last_rev    INTEGER NOT NULL,
    target_id   INTEGER NOT NULL REFERENCES(inodes.id),
    link_id     INTEGER NOT NULL REFERENCES(inodes.id),
    kind        ENUM('category', 'hyperlink', 'redirect')
);

-- Search index
CREATE TABLE search_documents (
    first_rev   INTEGER  NOT NULL,
    last_rev    INTEGER  NOT NULL,
    inode_id    INTEGER  NOT NULL REFERENCES(inodes.id),
    doc_name    VARCHAR  NOT NULL,
    document    TSVECTOR NOT NULL
)