Very roughly, what we have here is a filesystem with alternate data stream support and no
directory/filename distinction. The reference binding is to HTTP. Filesystem paths become
HTTP paths, and alternate data streams are accessed via the `attribute` query parameter.

Attributes are of the following types. Not sure if types need to be parts of names yet.
* **DATA**: Just ordinary bytes.
* **PROPS**: List of key/value pairs, string/JSON basically.
* **REL**: Directory contents
* **REV**: List of directory entries pointing at this indode.

Three attributes always exist (this assumes types are part of names):
* `content.data`: The normal entity body. Can be absent/empty, as in redirects.
* `meta.props`: Metadata in name/value form, such as WebDAV properties.
* `children.rel`: The children of this file, in the sense of a directory's children.
* `redirects.rev`: Lists files that are internal redirects to this one.

For a given path, GET returns a representation of that file as a whole.
* `?attribute` selects a specific attribute to examine
* `?format` is equivalent to an `Accept` header.
* `?display=render` gives you the browser-friendly form rather than the raw data.
* `?display=raw` forces the opposite (for completeness).

Methods with `?attribute` specified:
* GET, PUT, PATCH work as expected, but don't PUT/PATCH with `?display=render`
* Everything else probably won't.

On the file itself:
* GET might give you an amalgamation rather than what was PUT to `content.data`.
* PUT/PATCH affect the same composite entity. Batching!
* PROPFIND/PROPPATCH affect `meta.props`
* COPY/MOVE/DELETE work as expected
* MKCOL creates a blank file (effectively, it's PUT that only works on nonexistent files)
* UNBIND is a synonym of DELETE
* REBIND is a synonym of MOVE
* MKREDIRECTREF and UPDATEREDIRECTREF do as usual too.

The wiki implementation of this stuff includes a few more:
* `description.data`: If `content.data` is not wikitext, this is wikitext describing it (for file uploads and the like).
* `meta.props`: Overrides for OEmbed etc go in here too.
* `category_members.rel`: All the pages that list this one as a category.
* `categories.rev`: All the pages this one lists as categories
* `whatlinkshere.rev`: All the pages that link to this one.

