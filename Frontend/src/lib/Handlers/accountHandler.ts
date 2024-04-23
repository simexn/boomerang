import { browser } from "$app/environment";
import { getToken } from "$lib/handlers/authHandler";
import { updateUserInfo } from "$lib/stores/userInfoStore";
import { HubConnectionBuilder } from '@microsoft/signalr';
import { tick } from "svelte";
import { writable } from "svelte/store";


const backendUrl = import.meta.env.VITE_BACKEND_URL;



export interface User{
    id: number;
    userName: string;
    email: string;
    accountCreated: string;
    profilePictureUrl: string;
    birthdate?: Date;
    pronouns: string;
    isAdmin: boolean;

}

export async function fetchUserInfo() {
    let token = await getToken();
    const response = await fetch(`${backendUrl}/account/getUserInfo`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    const data = await response.json();

    if (response.ok && data.userInfo) {
        console.log("userinfo is" + data.userInfo.isAdmin)
        const user: User = {
            id: data.userInfo.id,
            userName: data.userInfo.userName,
            email: data.userInfo.email,
            accountCreated: new Date(data.userInfo.accountCreated).toLocaleString(),
            pronouns: data.userInfo.pronouns,
            birthdate: new Date(data.userInfo.birthdate),
            profilePictureUrl: `${backendUrl}${data.userInfo.profilePictureUrl}?${Date.now()}`,
            isAdmin: data.userInfo.isAdmin
        }
        console.log("user is admin: "+ user.isAdmin)
        return user;   
    }
    else{
        const user: User = {
            id: 0,
            userName: '',
            email: '',
            accountCreated: '',
            pronouns: '',
            profilePictureUrl: '',
            isAdmin: false
        }
        return user;
    }
}

export async function fetchUserId(){
    let token = await getToken();
        const response = await fetch(`${backendUrl}/account/getUserId`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        const data = await response.json();
    
        if(response.ok){
            return data.userId;
        }
}

export async function fetchActiveUsers(){
    let token = await getToken();
    const response = await fetch(`${backendUrl}/account/getActiveUsers`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    const data = await response.json();

    if(response.ok){
        return data.activeUsers;
    }

}

export async function isLoggedIn() {
    let cookie;
    if (browser) {
        cookie = document.cookie.split('; ').find(row => row.startsWith('token'));
    }
    if (!cookie) {
        console.log('Token not found isLogged');
        return false;
    }

    // Send a request to the IsUserLoggedIn endpoint
    const response = await fetch(`${backendUrl}/auth/isUserLoggedIn`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${await getToken()}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    // Check the response
    if (response.ok) {
        return true;
    } else {
        console.error('Failed to check if user is logged in:', await response.text());
        return false;
    }
}

export async function handleAccountRegister(formData: { username: string, email: string, password: string, confirmPassword: string, birthDate: Date, pronouns: string }) {


    const requestBody = JSON.stringify(formData);
  
    const response = await fetch(`${backendUrl}/account/register`, {
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

    const data = await response.json();
    
    return data;
}

export async function handleAccountLogin(formData:{usernameEmail: string, password: string}) {
    const requestBody = JSON.stringify(formData);
    const response = await fetch(`${backendUrl}/auth/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody
    });

    if (!response.ok) {
        const errorData = await response.json();
        console.error('Error data:', errorData);
        throw new Error(response.statusText);
    }

    const data = await response.json();
    if (data.token){
        document.cookie = `token=${data.token};path=/;Secure;SameSite=Strict;`;
    } 
    return data;
}

export async function handleUpdateInformation(formData: {username: string, email: string, birthdate:Date, pronouns: string, password: string, confirmPassword: string}) {
    let token = await getToken();
    const requestBody = JSON.stringify(formData);
    const response = await fetch(`${backendUrl}/account/updateInformation`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: requestBody
    });

    if (response.ok) {
        await updateUserInfo();
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.json();
    return data;

}

export function validateUsername(username: string) {
    if (username.length === 0) {
        return ' ';
    } else if (!/^[a-zA-Z]/.test(username)) {
        return 'Името трябва да започва с латинска буква.';
    } else if (!/^[a-zA-Z0-9]*$/.test(username)) {
        return 'Потребителското име може да съдържа само латински букви и цифри.';
    } else if (username.length > 20) {
        return 'Потребителското име е прекалено дълго.';
    } else if(username.length < 4){
        return 'Потребителското име трябва да е поне 4 символа.';
    } else {
        return ' ';
    }
}

export function validateEmail(email: string) {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    if (email.length === 0) {
        return ' ';
    } else if (!emailRegex.test(email)) {
        return 'Невалиден имейл адрес.';
    } else {
        return ' ';
    }
}

export function validatePassword(password: string) {

    if (password.length === 0) {
        return ' ';
    } else if (password.length < 6) {
        return 'Паролата трябва да съдържа поне 6 символа.';
    } else if (!/[A-Z]/.test(password)){
        return 'Паролата трябва да съдържа поне една главна буква.';
    } else if (!/[0-9]/.test(password)){
        return 'Паролата трябва да съдържа поне една цифра.';
    } else if (!/[!@#$%^&*]/.test(password)){
        return 'Паролата трябва да съдържа поне един специален символ.';
    } else {
        return ' ';
    }
}

export function validateConfirmPassword(password: string, confirmPassword: string) {
    if (confirmPassword.length === 0) {
        return ' ';
    } else if (confirmPassword !== password) {
        return 'Паролите не съвпадат.';
    } else {
        return ' ';
    }
}



