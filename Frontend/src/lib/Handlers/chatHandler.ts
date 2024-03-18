import {getToken} from './authHandler';
const backendUrl = import.meta.env.VITE_BACKEND_URL;

export interface Message{
    id: number;
    message: string;
    chatId: number;
    fromUser: number;
    fromUserId: number;
    date: string;
    }

export interface ChatItem {
    id: number;
    content: string;
    timestamp: string;
    userName: string;
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
            const chatItems: ChatItem[] = await data.chatItems.map((item: any) => ({
                id: item.id,
                content: item.content,
                timestamp: new Date(item.timestamp).toLocaleString(),
                userName: item.userName
            }));
    
            console.log(chatItems);
            return chatItems;
        } else {
            
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








