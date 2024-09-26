### Hasher
A very very simple code sample (no error handling or retry - THIS IS NOT PRODUCTION LEVEL CODE) to demonstrate how to divide a simple read  query workload across dynamically registered set of shards / partitioned workers on different urls with a partitition aggregation layer.

Arbiter - handles registration and maintains a list of locations of partition servers (Stateful) - should be multiple instance active-passive load balanced for HA as state will recover via the servers auto-re-registering
PartitionReader - performs the actualproxy reads across the appropriate partition servers and aggregates them back to the client - also acquires the list of partitions from the Arbiter in advance (Stateless) - should simply be scaled out, and could be globally load balanced.
Server - handles the buffering of the read workload (A list of hashes) and simply serves them accordingly, self - registers with the arbiter - these could be implemented as stateful sets or separate instances simply with varying configuration and each partition could scale out, possibly behind a geo-load balancer for lightning fast global scale

