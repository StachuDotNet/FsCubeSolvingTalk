type IEnumerator<'a> = 
    abstract member Current : 'a
    abstract MoveNext: unit -> bool

