import http from 'k6/http';


//  k6 run .\book_lt.js

export const options = {
    stages: [
        {duration: '20s', target: 100},
        {duration: '30s', target: 100},
        {duration: '20s', target: 0},
    ],
};

export default function () {
    let url = 'https://localhost:7191/v2/rooms/bookings';
    let body = JSON.stringify({
        "userId": "ECEE03DE-62E2-45B6-8A91-883B0487E4BC",
        "roomId": "B6EC726F-C13E-410B-B161-20D77B4617A8",
        "date": "2024-10-26",
        "start": "08:00:00",
        "end": "10:00:00"
    });
    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };
    let response = http.post(url, body, params);
}
