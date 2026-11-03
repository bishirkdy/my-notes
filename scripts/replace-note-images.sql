BEGIN;

WITH mapping(old_public_id, slug) AS (
  VALUES
    ('technotes/ado-net-transactions_ia1zo7', 'adonet-transactions'),
    ('technotes/ado-net-architecture_v9dev3', 'adonet-architecture'),
    ('technotes/angular-signals-change-detection_t96u7d', 'angular-signals-change-detection'),
    ('technotes/angular-http-interceptors_yzo1b5', 'angular-http-interceptors'),
    ('technotes/angular-di-injector-tree_xnosuk', 'angular-di-injector-tree'),
    ('technotes/angular-router-guards_zlxvon', 'angular-router-guards'),
    ('technotes/angular-bootstrap-architecture_k1ynls', 'angular-bootstrap-architecture'),
    ('technotes/angular-ngrx-flow_xrebyn', 'angular-ngrx-flow'),
    ('technotes/dotnet-execution_cusg4l', 'dotnet-execution'),
    ('technotes/aspnet-pipeline_ydkbeu', 'aspnet-pipeline'),
    ('technotes/dotnet-project-setup_uk4dmq', 'dotnet-project-setup'),
    ('technotes/di-lifetimes_xju5sh', 'di-lifetimes'),
    ('technotes/jwt-refresh_yvzop2', 'jwt-refresh'),
    ('technotes/csharp-async_lagcfq', 'csharp-async'),
    ('technotes/csharp-execution_ocke0d', 'csharp-execution'),
    ('technotes/csharp-type-system_h079vh', 'csharp-type-system'),
    ('technotes/csharp-linq_kjddwl', 'csharp-linq'),
    ('technotes/csharp-oop_ljjwyg', 'csharp-oop'),
    ('technotes/ef-core-migrations_xw97jq', 'ef-core-migrations'),
    ('technotes/ef-core-architecture_v5ojtl', 'ef-core-architecture'),
    ('technotes/javascript-prototype-chain_kkfpgj', 'javascript-prototype-chain'),
    ('technotes/javascript-event-loop_wzswe4', 'javascript-event-loop'),
    ('technotes/react-reconciliation_km4z9f', 'react-reconciliation'),
    ('technotes/react-hooks-lifecycle_ppr0px', 'react-hooks-lifecycle'),
    ('technotes/react-data-flow_yrnhgy', 'react-data-flow'),
    ('technotes/react-router-tree_xeb8ep', 'react-router-tree'),
    ('technotes/react-render-pipeline_a8cnns', 'react-render-pipeline'),
    ('technotes/react-fiber_dzvcuj', 'react-fiber'),
    ('technotes/redux-async-lifecycle_ueqmvc', 'redux-async-lifecycle'),
    ('technotes/redux-data-flow_ewrfmt', 'redux-data-flow'),
    ('technotes/tanstack-query-lifecycle_zt4mwn', 'tanstack-query-lifecycle'),
    ('technotes/tanstack-mutation-lifecycle_zzcv98', 'tanstack-mutation-lifecycle')
)
UPDATE "NoteContents" AS c
SET "Data" = c."Data" || jsonb_build_object(
  'url', '/illustrations/handdrawn/v1/' || m.slug || '.svg',
  'publicId', 'illustrations/handdrawn/v1/' || m.slug,
  'format', 'svg',
  'width', 1600,
  'height', 900
)
FROM mapping AS m
WHERE c."Data"->>'publicId' = m.old_public_id;

COMMIT;

SELECT
  count(*) FILTER (WHERE "Data"->>'url' LIKE '/illustrations/handdrawn/v1/%') AS local_images,
  count(*) FILTER (WHERE "Data"->>'url' LIKE '%cloudinary%') AS cloudinary_images
FROM "NoteContents";
