import http from 'k6/http';
import {check } from 'k6';

// k6 run --address "localhost:3000" .\inventory_lt.js

export const options = {
    stages: [
        { duration: '1s', target: 1 },
        { duration: '69s', target: 1 },
    ],
};

export default function () {
    let res = http.get('https://localhost:7191/v2/inventory');
    
    check(res, {
        'is valid inventory': (r) => r.body.includes('true')
    })
    
}