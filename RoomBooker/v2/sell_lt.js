import {SharedArray} from 'k6/data';
import { scenario } from 'k6/execution';
import http from 'k6/http';

// k6 run .\sell_lt.js

const data = new SharedArray('users', function () {
    return [
        "ECEE03DE-62E2-45B6-8A91-883B0487E4BC",
        "2C7ADDD3-059D-4682-9374-D5741B664E89"
    ]
});

export const options = {
    stages: [
        { duration: '10s', target: 20 },
        { duration: '60s', target: 20 },
    ],
};

export default function () {
    let url = 'https://localhost:7191/v2/articles/sell';
    let body = JSON.stringify({
        userId: data[scenario.iterationInTest % 2],
        id: "953D41C4-3AB1-4BD6-AC8F-4FA037E78125"
    });
    
    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };
    
    let response = http.post(url, body, params);
}
