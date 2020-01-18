export interface Basic {
    pathname: string;
    operation: string;
}

export interface Delete extends Basic {
    operation: "delete";
}

export interface Move extends Basic {
    operation: "move" | "copy";
    destinationPathname: string;
}

export interface Attribute extends Basic {
    operation: "get" | "put";
    attribute: string;
    attributeParts?: string[];
    attributeRanges?: {start: number, end?: number}[];
}

export interface Get extends Attribute {
    operation: "get";
}