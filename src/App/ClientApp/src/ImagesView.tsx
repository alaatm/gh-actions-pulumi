import React from 'react';
import useFetch from './useFetch';

interface Props {
    hash: string;
}

type Image = {
    id: number,
    name: string,
    size: number,
    data: Blob,
};

const ImagesView: React.FC<Props> = (props: Props) => {
    const data = useFetch("/images", props.hash) as Image[] | null;

    if (!data) {
        return <div>Loading...</div>;
    } else if (data.length) {
        return (
            <div className="card-group">{data.map(img => (
                <div key={img.id} className="card">
                    <img src={`data:image/png;base64, ${img.data}`} className="card-img-top" alt="..." />
                    <div className="card-body">
                        <h5 className="card-title">{img.name}</h5>
                        <p className="card-text">TBD</p>
                    </div>
                </div>
            ))}</div>
        );
    } else {
        return <p className="text-center">No data</p>;
    }
};

export default ImagesView;
