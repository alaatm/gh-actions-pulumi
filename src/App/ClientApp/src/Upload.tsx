import React, { useState } from 'react';

interface Props {
    imageUploaded: () => void;
}

const Upload: React.FC<Props> = (props: Props) => {

    const [file, setFile] = useState<File | null>(null);

    const handleUpload = async () => {
        if (file) {
            var formData = new FormData();
            formData.append(file.name, file);
            const response = await fetch('/images', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                alert('File uploaded.');
                setTimeout(() => props.imageUploaded(), 500);
            } else {
                alert('Error uploading file.');
            }
        }
    };

    return (
        <div className="row justify-content-center upload">
            <div className="col-md-auto">
                <input type="file" onChange={(e) => setFile(e.target.files ? e.target.files[0] : null)} />
            </div>
            <div className="col-md-auto">
                <button type="button" className="btn btn-primary" onClick={handleUpload}>Upload</button>
            </div>
        </div>
    );
};

export default Upload;
