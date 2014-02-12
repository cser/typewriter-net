Here's an inefficient but elegant quicksort program in Haskell:

> qsort :: (Enum a) => [a] -> [a]
> qsort []     = []
> qsort (x:xs) = qsort (filter (< x) xs) ++ [x] ++
>                qsort (filter (>= x) xs)

Here's a more efficient but less transparent version:

> qsort (x:xs) y = part xs [] [x] []  
>     where
>         part [] l e g = qsort l (e ++ (qsort g y))
>         part (z:zs) l e g 
>             | z > x     = part zs l e (z:g) 
>             | z < x     = part zs (z:l) e g 
>             | otherwise = part zs l (z:e) g