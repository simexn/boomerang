import { getToken } from './authHandler';

export interface Group {
    id: number;
    name: string;
    isGroup: boolean
    creatorId: number;
    inviteCode: string;
}

function generateInviteCode(length: number) {
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() * characters.length));
    }
    return result;
}

export async function fetchGroupInfo(groupId: string){
    let token = await getToken();
    
    const response = await fetch(`https://localhost:5000/group/getGroupInfo?chatId=${groupId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    
    const data = await response.json();
    
    if (response.ok && data.chat) {
        const groupInfo: Group = {
            id: data.chat.id,
            name: data.chat.name,
            isGroup: data.chat.isGroup,
            creatorId: data.chat.creatorId,
            inviteCode: data.chat.inviteCode
        }
        return groupInfo;
    } else {
        console.error('Error fetching chat:', await response.text());
        return null;
    }

    
}

export async function fetchChats() {
    try {
        let token = await getToken();
        
        const response = await fetch('https://localhost:5000/chat/getAllChats' , {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        
        return data.chats;
    } catch (error) {
        console.error('Error fetching chats:', error);
    }
};

export async function handleRoomSubmit(formData: {newGroupName: string, inviteCode: string}) {
    let token = await getToken();
    
    if (formData.inviteCode === '') {
        formData.inviteCode = generateInviteCode(6);
    }

    const requestBody = JSON.stringify(formData);

    const response = await fetch('https://localhost:5000/chat/createRoom', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody 
    });

    if (response.ok){
        var chats = await fetchChats();
        return chats;
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}

export async function handleJoinRoom(inviteCode: string) {
    let token = await getToken();
    

    const response = await fetch('https://localhost:5000/group/joinGroup', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify(inviteCode)
    });

    if (response.ok){
        var chats = await fetchChats();
        return chats;
    }
    if (!response.ok) {
        console.log("losh otgovor");
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}

export async function handleLeaveGroup(chatId: string) {
    const token = await getToken();

    const response = await fetch(`https://localhost:5000/group/leaveGroup/${chatId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        const message = `An error has occurred: ${response.status}`;
        throw new Error(message);
    }

    return response;
}

export async function handleDeleteGroup(chatId: string) {
    const token = await getToken();

    const response = await fetch(`https://localhost:5000/group/deleteGroup/${chatId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
    });

    if (!response.ok) {
        const message = `An error has occurred: ${response.status}`;
        throw new Error(message);
    }

    return response;
}



