import { getToken } from "./authHandler";
const backendUrl = import.meta.env.VITE_BACKEND_URL;

export interface UserModerate{
    id: number;
    username: string;
    email: string;
    profilePictureUrl: string;
    accountCreatedDate: string;
    birthDate: string;
    pronouns: string;
    isAdmin: boolean;
}
export async function handleFetchUserModerate(identificator: string){
    const token = await getToken();
    const response = await fetch(`${backendUrl}/admin/getUserModerate/${identificator}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });
    if (response.ok){
        const data = await response.json();
        const userModerate: UserModerate = {
            id: data.id,
            username: data.username,
            email: data.email,
            profilePictureUrl: data.profilePictureUrl,
            accountCreatedDate: data.accountCreatedDate,
            birthDate: data.birthDate,
            pronouns: data.pronouns,
            isAdmin: data.isAdmin
        }
        return userModerate;
    }
    else{
        return {} as UserModerate;
    }
}

export async function handleUpdateUser(formData: {
    id: number;
    username: string;
    email: string;
    birthDate: Date;
    pronouns: string;
    profilePictureUrl: string;
    isAdmin: boolean;
    newPassword?: string;
}) {
    const token = await getToken();
    const response = await fetch(`${backendUrl}/admin/updateUser`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
    });

    if (response.ok) {
        const text = await response.text();
        return text ? JSON.parse(text) : {};
    } else {
        throw new Error(await response.text());
    }
}

export async function handleDeleteUser(id: number) {
    const token = await getToken();
    const response = await fetch(`${backendUrl}/admin/deleteUser/${id}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        const text = await response.text();
        return text ? JSON.parse(text) : {};
    } else {
        throw new Error(await response.text());
    }
}