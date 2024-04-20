import { browser } from "$app/environment";


export async function getToken() {
    let cookie;
    if (browser) {
        cookie = await document.cookie.split('; ').find(row => row.startsWith('token'));
    }
    if (!cookie) {
        console.log('Token not found getToken');
        return '';
    }

    const token = cookie.split('=')[1];
    return token;
}