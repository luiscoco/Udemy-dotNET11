# Observed MTP policies

The global.json file selects Microsoft.Testing.Platform. The timeout action uses --timeout 5s and must return SDK exit code 3. The failure-limit action uses --maximum-failed-tests 1 and must return SDK exit code 13. A stop request may leave work already in flight. The passing solution run uses --results-directory-layout per-module and --report-trx-filename results.trx so each module writes its own file.
