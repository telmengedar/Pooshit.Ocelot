SELECT 'CREATE TABLE ' || pn.nspname || '.' || pc.relname || E '(\n' ||
   string_agg('"' || pa.attname || '"' || ' ' || pg_catalog.format_type(pa.atttypid, pa.atttypmod) || 
   coalesce(' DEFAULT ' || (
                   SELECT pg_catalog.pg_get_expr(d.adbin, d.adrelid)
                   FROM pg_catalog.pg_attrdef d
                   WHERE d.adrelid = pa.attrelid
                     AND d.adnum = pa.attnum
                     AND pa.atthasdef
                   ),
     '') || ' ' ||
              CASE pa.attnotnull
                  WHEN TRUE THEN 'NOT NULL'
                  ELSE 'NULL'
              END, E',\n') ||
   coalesce((SELECT E',\n' || string_agg('CONSTRAINT ' || pc1.conname || ' ' || pg_get_constraintdef(pc1.oid), E',\n' ORDER BY pc1.conindid)
            FROM pg_constraint pc1
            WHERE pc1.conrelid = pa.attrelid), '') ||
   E');' as definition
FROM pg_catalog.pg_attribute pa
         JOIN pg_catalog.pg_class pc
              ON pc.oid = pa.attrelid
                  AND pc.relname = '{0}'
         JOIN pg_catalog.pg_namespace pn
              ON pn.oid = pc.relnamespace
                  AND pn.nspname = 'public'
WHERE pa.attnum > 0
  AND NOT pa.attisdropped
GROUP BY pn.nspname, pc.relname, pa.attrelid;