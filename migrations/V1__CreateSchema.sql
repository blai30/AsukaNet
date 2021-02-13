/*
 Ensure the schema has the correct encoding.
 */

CREATE SCHEMA IF NOT EXISTS asuka_net
    DEFAULT CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

ALTER SCHEMA asuka_net
    DEFAULT CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
