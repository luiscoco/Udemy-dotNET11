public static class RetrievalQueries
{
    public static string Build(string kind,string table,string filter,int dimensions) => kind switch
    {
        "keyword" => $"""
            SELECT id,title,body,url,
                   ts_rank_cd(search,websearch_to_tsquery('english',@text))::float8
            FROM {table}
            WHERE ({filter}) AND search @@ websearch_to_tsquery('english',@text)
            ORDER BY 5 DESC,id LIMIT @k
            """,
        "hybrid" => $"""
            WITH v AS (
                SELECT id,row_number() OVER(ORDER BY embedding <=> @q::vector,id) r
                FROM {table} WHERE ({filter})
                ORDER BY embedding <=> @q::vector,id LIMIT 10
            ), t AS (
                SELECT id,row_number() OVER(
                    ORDER BY ts_rank_cd(search,websearch_to_tsquery('english',@text)) DESC,id) r
                FROM {table} WHERE ({filter})
                  AND search @@ websearch_to_tsquery('english',@text) LIMIT 10
            ), fused AS (
                SELECT coalesce(v.id,t.id) id,
                       coalesce(1.0/(60+v.r),0)+coalesce(1.0/(60+t.r),0) score
                FROM v FULL JOIN t USING(id)
            )
            SELECT d.id,d.title,d.body,d.url,f.score::float8
            FROM fused f JOIN {table} d USING(id)
            ORDER BY f.score DESC,d.id LIMIT @k
            """,
        "reranked" => $"""
            WITH candidates AS MATERIALIZED (
                SELECT * FROM {table} WHERE ({filter})
                ORDER BY binary_quantize(embedding)::bit({dimensions})
                    <~> binary_quantize(@q::vector)::bit({dimensions}),id LIMIT 6
            )
            SELECT id,title,body,url,(1-(embedding <=> @q::vector))::float8
            FROM candidates ORDER BY embedding <=> @q::vector,id LIMIT @k
            """,
        _ => $"""
            SELECT id,title,body,url,(1-(embedding <=> @q::vector))::float8
            FROM {table} WHERE ({filter})
            ORDER BY embedding <=> @q::vector,id LIMIT @k
            """
    };
}
