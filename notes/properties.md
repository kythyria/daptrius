Some properties (in meta.props) are basically standard.

# Filesystem-related

## fs.redirect
String URL. If set, this file is a redirect to someplace. Setting it while the file isn't
empty is an error.

## fs.acl
ACL.

## fs.owner
UID. If the access control engine or user procedures demand that someone be identified as
the owner of a file, this property lists who that is.

## fs.attributes
Catalogues the attributes that this file has, their sizes, and URLs.

## fs.name
The name of the file, of course

## fs.stable_url
URL of this file, by inode number or similar rather than path.

# Wiki/CMS related

## cms.oembed
JSON object overriding oembed responses for this page. Not precisely OEmbed format.

## cms.approver
UID of whoever approved this revision for visibility

## cms.approval_date
Timestamp of when the revision was marked visible