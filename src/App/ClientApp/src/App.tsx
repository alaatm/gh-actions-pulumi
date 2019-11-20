import React, { useState } from 'react';
import uuidv1 from 'uuid';
import Container from './Container';
import Upload from './Upload';
import ImagesView from './ImagesView';

import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css';

const App: React.FC = () => {
    const [hash, setHash] = useState('');

    return (
        <Container>
            <Upload imageUploaded={() => setHash(uuidv1())} />
            <hr />
            <ImagesView hash={hash} />
        </Container>
    );
}

export default App;
