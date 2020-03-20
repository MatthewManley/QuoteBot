-- This change was to allow quotes to below to multiple categories
BEGIN TRANSACTION;

-- First rename the current audio table
ALTER TABLE audio RENAME TO _audio_old;

-- Create the new audio table, this no longer has a category, instead it has an id
CREATE TABLE audio (
    id  INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    path TEXT NOT NULL
);

-- Create a category table
CREATE TABLE category (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL
);

-- Create a relationship category that pairs audio with categories
CREATE TABLE audio_category (
    audio INTEGER NOT NULL,
    category INTEGER NOT NULL
);

INSERT INTO category (name)
    SELECT DISTINCT category FROM _audio_old;

INSERT INTO audio (name, path)
    SELECT name, path FROM _audio_old;

INSERT INTO audio_category (audio, category)
    SELECT audio.id, category.id FROM _audio_old
    INNER JOIN audio
        ON audio.name = _audio_old.name AND audio.path = _audio_old.path
    INNER JOIN category
        ON category.name = _audio_old.category;

DROP TABLE _audio_old;

PRAGMA user_version = 2;

COMMIT;
