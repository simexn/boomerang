<script lang="ts">
    import type {ChatItem} from "$lib/Handlers/chatHandler";
    import type { User } from "$lib/Handlers/accountHandler";
    import { writable } from "svelte/store";
    import { isLoggedIn } from '$lib/Handlers/accountHandler';
    import { userStatuses } from '$lib/stores/userStatusesStore';
    import { onMount } from "svelte";
    import { getToken } from "$lib/Handlers/authHandler";
    import { HubConnectionBuilder } from "@microsoft/signalr";
    export let item: ChatItem;
    export let imageUrl: string;
    export let userInfo: User;
    export let isEditingMessage = (id: number, content: string) => {};
    export let deleteMessage = (id: number) => {};
    export let confirmEdit = (id: number, newContent: string) => {};
    export let cancelEdit = (id: number) => {};
    export let isEditing = writable();

    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    onMount(async () => {
        if(await isLoggedIn()){
            
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
                let connection = new HubConnectionBuilder()
                    .withUrl(`${backendUrl}/accountHub`)
                    .build();

                    connection.on("UpdateUserStatus", async (userId, status) => {
                        console.log("userid" + userId + "status" + status)
                        
                        userStatuses.update(statuses => ({ ...statuses, [userId]: status }));
                    });

                    await connection.start();

                    let userId = data.userId.toString();

                    await connection.invoke("UpdateUserStatus", userId, "online");
                    }
            }
    });
</script>



<div id="img">
    <button class="message-avatar-wrap">
        <span class="message-avatar">
            <img style="width:32px; height:32px;" alt="User" src={imageUrl}>
        </span>
        <span class="status-dot" class:online={$userStatuses[item.userId.toString()] == 'online'}></span>
    </button>
</div>
<div>
    <div class="message-header">
        <div class="message-sender">
            <button class="message-sender-button">{item.userName}</button>                            
        </div>
        <div class="message-date">
            <a href="/" class="message-date-button" style="display: inline-block;color: inherit;">{item.time}</a>
        </div> 
    </div>   
    <div class="message-body">                    
        {#if $isEditing === item.id}
            <p class="message-edit-info" style=""><i>currently editing message:</i></p>
            <input class="message-editing" bind:value={item.content}/><br>
            <a class="message-edit-confirm" on:click={() => confirmEdit(item.id, item.content)} href="#"><b>save</b></a>
            <a class="message-edit-confirm" on:click={() => cancelEdit(item.id)} href="#"><b>cancel</b></a>
        {:else if item.isDeleted == true}
            <p style="color: grey;"><i>This message has been deleted.</i></p>
        {:else if item.isEdited == true}
            <p style="display: inline-block;">{item.content}</p>
            <p style="display: inline-block; font-size: 12px; color: #B8B8B8;"><i>(edited)</i></p>
        {:else}
            <p>{item.content}</p>
        {/if}                                       
    </div>
    
    {#if userInfo && item.userName === userInfo.userName && $isEditing !== item.id && !item.isDeleted}
        <div class="message-actions">
            <i class="icon-edit fa fa-pencil" on:click={() => isEditingMessage(item.id, item.content)}></i>
            <i class="icon-delete fa fa-trash" on:click={() => deleteMessage(item.id)}></i>
        </div>
    {/if}
</div>

<style>
    .status-dot {
    height: 10px;
    width: 10px;
    background-color: #bbb;
    border-radius: 50%;
    display: inline-block;
    }
    .status-dot.online {
    background-color: #4CAF50;
    }
    .message-actions {
    position: absolute;
    top: -5px;
    right: 0;
    margin-right: .5em;
    display: flex;
    justify-content: flex-end;
    padding: 4px;
    visibility: hidden;
    border:solid 1px;
    border-radius: 15px;
    border-color: rgba(63, 67, 80, 0.2);
    background-color: white;
    
    justify-content: space-evenly;
}



.icon-edit {
    padding: 6px 8px 6px 8px;
    margin-left: 8px;
    margin-right:4px;
    cursor: pointer;
    border-radius: 8px;
    font-size: 1.2em;
}
    .icon-edit:hover{
        background-color: #dddddd;
        color:var(--prim-tp);
    }
.icon-delete{
    padding: 6px 8px 6px 8px;
    margin-right:4px;
    margin-left:8px;
    cursor: pointer;
    border-radius: 8px;
    font-size: 1.2em;
    
}
    .icon-delete:hover{
        background-color: #dddddd;
        color: red;
    }


    
.message-header{
    position:relative;
    display: flex;
    width: 100%;
    margin-bottom: 2px;
    white-space: nowrap;
}
    .message-avatar-wrap{
        position: relative;
        display: inline-block;
        height: 32px;
        padding: 0;
        border: none;
        background: transparent;
    }
        .message-avatar{
            display: inline-flex;
            overflow: hidden;
            align-items: center;
            justify-content: center;
        }
        #img {
        min-width: 40px; /* adjust as needed */
        flex-shrink: 0;
        }
    .message-sender{
        display: flex;
        min-width: 0;
        flex: 0 auto;
        margin-right: 8px;
        font-weight: 600;
        overflow: hidden;
        text-overflow: ellipsis;
    }
        .message-sender-button{
            border: none;
            background: transparent;
            font-size: 18px;
            font-weight: bold;
        }
    .message-date{
        max-width: 100%;flex-basis: 0;flex-grow: 1;
    }
        .message-date-button{
            display: inline-block;
            font-size: 1em;
            opacity: .6;
            margin-top: .18em;
            text-decoration: none;
        }

        

.message-body{
    transition-property: height ;transition-duration: 250ms;transition-timing-function: ease;height: auto;overflow: visible;
    width: 100%; word-wrap:break-word;
    position: relative;overflow: clip;
    max-height:600px;
    text-align: left;
    margin-left:5px;


}
.message-editing{
    border-style: solid;
    border-color: #eeeeee;
    border-radius: 10px;

    background-color: var(--sec-fg);
}
.message-edit-info{
    margin:0;
    font-size:12px; 
    color: #acacac;
}

.message-edit-confirm{
    text-decoration:none;
     font-size: 15px;
    margin-left:2.5px;
    margin-right:2.5px;
    cursor:pointer;
}
</style>