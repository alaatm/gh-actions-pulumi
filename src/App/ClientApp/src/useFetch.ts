import { useState, useEffect } from 'react'

const useFetch = (url: string, hash: string) => {
    const [data, setData] = useState(null);

    async function fetchData() {
        const response = await fetch(url);
        const json = await response.json();
        setData(json);
    }

    useEffect(() => { fetchData() }, [url, hash]);
    return data;
};

export default useFetch;