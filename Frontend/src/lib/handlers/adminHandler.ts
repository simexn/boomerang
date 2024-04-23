import { getToken } from "./authHandler";

const backendUrl = import.meta.env.VITE_BACKEND_URL;
export interface AdvancedUser{
    id:number;
    username: string;
    email: string;
    accountCreatedDate: string;
    birthDate: string;
    pronouns: string;
    profilePictureUrl:string;
    IsAdmin: boolean;
};

export async function fetchAdvancedUserInfo(input: string){
    let token = await getToken();
    const response = await fetch(`${backendUrl}/admin/getUserInfo/${input}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    const data = await response.json();

    if (response.ok && data.userInfo) {
        const user: AdvancedUser = {
            id: data.userInfo.id,
            username: data.userInfo.username,
            email: data.userInfo.email,
            accountCreatedDate: new Date(data.userInfo.accountCreatedDate).toLocaleString(),
            pronouns: data.userInfo.pronouns,
            birthDate: new Date(data.userInfo.birthDate).toLocaleString(),
            profilePictureUrl: `${data.userInfo.profilePictureUrl}`,
            IsAdmin: data.userInfo.IsAdmin
        }
        return user;   
    }
    else{
        const user: AdvancedUser = {
            id: 0,
            username: '',
            email: '',
            accountCreatedDate: '',
            pronouns: '',
            birthDate: '',
            profilePictureUrl: '',
            IsAdmin: false
        }
        return user;
    }
}