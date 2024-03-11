import { browser } from "$app/environment";
import { getToken } from "$lib/Handlers/authHandler";

export interface User{
    id: number;
    userName: string;
    email: string;
    accountCreated: string;

}

export async function fetchUserInfo() {
    let token = await getToken();
    const response = await fetch('https://localhost:5000/account/getUserInfo', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    const data = await response.json();

    if (response.ok && data.userInfo) {
        
        const user: User = {
            id: data.userInfo.id,
            userName: data.userInfo.userName,
            email: data.userInfo.email,
            accountCreated: new Date(data.userInfo.accountCreated).toLocaleString()
        }
        console.log(user);
        return user;   
    }
    else{
        return null;
    }
}

async function isLoggedIn() {
    let cookie;
    if (browser) {
        cookie = document.cookie.split('; ').find(row => row.startsWith('token'));
    }
    if (!cookie) {
        console.log('Token not found');
        return false;
    }

    
    return true;
}

export async function handleAccountRegister(formData: { username: string, email: string, password: string, confirmPassword: string, birthDate: Date, pronouns: string }) {

    console.log(formData);
    const requestBody = JSON.stringify(formData);
  
    const response = await fetch('https://localhost:5000/account/register', {
        method: 'POST',
        headers: {    
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody
    });

    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
    
    return response;
}

export async function handleAccountLogin(formData:{email: string, password: string}) {
    const requestBody = JSON.stringify(formData);
    const response = await fetch('https://localhost:5000/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody
    });

    const data = await response.json();

    if (response.ok) {
        document.cookie = `token=${data.token};path=/;Secure;SameSite=Strict;`;
        window.location.href = '/chat/home'; // Redirect to home page
        
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }


    return response;
}

