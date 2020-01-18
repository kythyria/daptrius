Access control is MAC-like: central policy scripts control who has access to what, rather
than detailed ACLs on each item. In version 1, these are set up at configuration time and
cannot be altered from the GUI (this is mostly because of the amount of infrastructure it
would take to give nice UX and safe execution otherwise).

Policy scripts are modules conforming to the `VolumePolicy` interface. In v1, this is two
functions, one which returns whether a given user is allowed to even see the volume, plus
one which returns what a given user is allowed to do to a specific file. It's broken into
two like that since the former function can be called for nonexistent paths.