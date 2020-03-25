BEGIN TRANSACTION;

ALTER TABLE audio RENAME TO _audio_old;
ALTER TABLE category RENAME TO _category_old;
ALTER TABLE audio_category RENAME TO _audio_category_old;

CREATE TABLE audio (
    id INTEGER PRIMARY KEY,
    path TEXT NOT NULL,
    uploader TEXT NOT NULL
);

CREATE TABLE category (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    owner TEXT NOT NULL
);

CREATE TABLE audio_owner (
    id INTEGER PRIMARY KEY,
    audio INTEGER NOT NULL,
    owner TEXT NOT NULL,
    name TEXT NOT NULL
);

CREATE TABLE audio_category (
    category INTEGER NOT NULL,
    audio_owner INTEGER NOT NULL
);

INSERT INTO audio (id, path, uploader)
    SELECT id, path, "107649869665046528" FROM _audio_old;

INSERT INTO category (id, name, owner)
    SELECT id, name, "178546341314691072" FROM _category_old;

INSERT INTO audio_owner (id, audio, owner, name)
    SELECT id, id, "178546341314691072", name FROM _audio_old;

INSERT INTO audio_category (category, audio_owner)
    SELECT category, audio FROM _audio_category_old;

DROP TABLE _audio_old;
DROP TABLE _category_old;
DROP TABLE _audio_category_old;

PRAGMA user_version = 3;

COMMIT;