This will eventually be some kind of wiki slash CMS because why not.

# Design goals
* WYSIWYG editing: ideally you should be able to click edit and the only difference
  is that now you have a cursor and toolbar.
  
* No mandatory JS for read-only access.

* Single-sign-on: get authentication *including group membership* from an external
  service rather than make everyone remember yet another username and password and
  admins keep yet another role table up to date.

* Be able to hold at least some binary files (images, mainly).

# Non-goals
Some of these might creep back in in the future.

* Be a conventional static site generator that works from an easy-to-edit directory
  tree with YAML front matter and the like. Previous attempts at this project
  failed partly because of the complexity of the on-disk layout involved.

* Have fancy stuff with templates and scripts like Mediawiki does. This was the
  other main reason previous attempts failed. While undoubtedly useful, they
  really need a *lot* of special handling.

* Support *really* big files (more than a few megabytes). Those should be put
  either on a generic host with good support for huge files, or one specialised
  for the handling a particular kind of content needs (re-encoding/muxing video
  for DASH compatibility, generating tile pyramids for huge images, etc).

# Design

See also `notes/` for stream of consciousness rambling.