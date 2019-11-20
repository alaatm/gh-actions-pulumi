import React from 'react';

const Container = (props: React.Props<{}>) => (
    <div className="container">
        <h1 className="text-center">Github Actions Test Drive</h1>
        <hr />
        {props.children}
        <hr />
    </div>
);

export default Container;
