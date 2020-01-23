export type PathSpec = string[] | number;

export interface Basic {
    status: string;
    path: PathSpec;
}

export interface NotFound {
    status: "notFound";
}