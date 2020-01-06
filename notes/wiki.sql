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

-----
-- Core content
-----

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

CREATE TABLE path_components (
    id         INTEGER PRIMARY KEY,
    first_rev  INTEGER NOT NULL,
    last_rev   INTEGER NOT NULL,
    slug       VARCHAR NOT NULL,
    parent     INTEGER          REFERENCES(path_components.id),
    inode      INTEGER          REFERENCES(inodes.id),
    kind       enum('redirect','hard') NOT NULL -- Is this a cross-reference that should be redirected to the one marked hard?
)

CREATE TABLE inodes (
    id         INTEGER PRIMARY KEY,
    first_rev  INTEGER NOT NULL,
    last_rev   INTEGER NOT NULL,
    title      VARCHAR NOT NULL
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
    kind        ENUM('category', 'hyperlink')
);

-- Search index
CREATE TABLE search_documents (
    first_rev   INTEGER  NOT NULL,
    last_rev    INTEGER  NOT NULL,
    inode_id    INTEGER  NOT NULL REFERENCES(inodes.id),
    doc_name    VARCHAR  NOT NULL,
    document    TSVECTOR NOT NULL
)