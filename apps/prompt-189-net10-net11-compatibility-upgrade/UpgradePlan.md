# Archive reader migration

Inventory the SDK, target framework, runtime, deployment RID and dependency versions. The sample depends on the base libraries.

Publish the original net10.0 target and record valid and CRC-mismatched input behavior. Retarget the same source to net11.0, consult the documented ZIP CRC32 compatibility change, handle InvalidDataException at the import boundary, and verify both valid and rejected input. Keep the previous artifact and verify consumer contracts before rollout.
