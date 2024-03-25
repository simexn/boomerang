import {getToken} from './authHandler';
const backendUrl = import.meta.env.VITE_BACKEND_URL;

export interface Message{
    id: number;
    message: string;
    chatId: number;
    fromUser: number;
    fromUserId: number;
    date: string;
    isEdited: boolean;
    }

export interface ChatItem {
    id: number;
    content: string;
    timestamp: string;
    time: string;
    date: string;
    userName: string;
    userId: string;
    isEdited?:boolean;
    isDeleted?:boolean;
    isEvent?:boolean;
}

export async function fetchMessages(chatId: string){
    let token = await getToken();
    
    const response = await fetch(`${backendUrl}/chat/getMessages?chatId=${chatId}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });
    
    const data = await response.json();
    
    
    if (response.ok) {
        if (data.chatItems) {
            const chatItems: ChatItem[] = await data.chatItems.map((item: any) => {
                const dateObject = new Date(item.timestamp);
                const date = dateObject.toLocaleDateString();
                const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);
                return {
                    id: item.id,
                    content: item.content,
                    timestamp: item.timestamp,
                    date: date,
                    time: time,
                    userName: item.userName,
                    userId: item.userId,
                    isEdited: item.isEdited,
                    isDeleted: item.isDeleted,
                    isEvent: item.isEvent
                };
            });
    
            
            return chatItems;
        } else {
            const chatItems: ChatItem[] = await data.chatItems.map((item: any) => ({
                id: "",
                content: "",
                timestamp: "",
                userName: "",
                isEdited: ""
            }));
            return [];
        }
        
    } else {
        throw new Error(data);
    }
};



export async function handleMessageSubmit(messageToSubmit: string, chatId: string, roomId: string){
    let token = await getToken();

    const array = [messageToSubmit, chatId, roomId]
    

    const requestBody = JSON.stringify(array);

    const response = await fetch(`${backendUrl}/chat/sendMessage`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: requestBody
    });

    if (!response.ok) {
        console.error('Error sending message:', await response.text());
    }
}

export async function handleEditMessage(chatId: string, messageId: number, newContent: string) {
    let token = await getToken();
    

    const response = await fetch(`${backendUrl}/chat/editMessage/${chatId}/${messageId}/${newContent}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    if (response.ok){
        return response;
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}
export async function handleDeleteMessage(chatId: string, messageId: number) {
    let token = await getToken();
    

    const response = await fetch(`${backendUrl}/chat/deleteMessage/${chatId}/${messageId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        credentials: 'include'
    });

    if (response.ok){
        return response;
    }
    if (!response.ok) {
        return response.text().then(text => { throw new Error(text) });
    }

    const data = await response.text();
}








