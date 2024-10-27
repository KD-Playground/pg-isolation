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
    let url = 'https://localhost:7191/v1/rooms/bookings';
    let body = JSON.stringify({
        "userId": "9A841211-47E1-4DE1-8628-9EFB9E811162",
        "roomId": "CEE3BBCD-BD83-4175-B30B-8233D26FFDDF",
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
