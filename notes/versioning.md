Versioning is basically linear: Each time something changes the revision number increases
for the whole volume. To prevent intermediate states, or ones that haven't been vetted by
a moderator, from becoming visible, there's also a concept of approved changes. These are
somewhat like cherry-picking into a separate branch, so as to permit approving changes to
one specific page without approving unrelated changes by accident.

Internally, each version of a row records the range of revisions to which it applies, and
also the range of approvals to which it applies. But the latter fields are nullable, with
only rows that have non-null values considered to be approved.